param(
    [string] $groupName = "zkj-test-group",
    [string] $appName = "zkjAzureTest",
    [string] $subscription = "Development",
    [validateset("all", "group-deploy", "template-deploy", "code-build", "code-deploy")]
    [string[]] $deployFlags
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$flags = $deployFlags
if(-not $flags)
{
    $flags = @( "all" )
}
$tf = ".\azuredeploy.json"
$webJob1Name = "ServiceBusProcessor1"
$webJob2Name = "ServiceBusProcessor2"

if($appName -notmatch '^[a-zA-Z]{0,18}$')
{
    throw "`$appName parameter with value '$appName' is invalid must match regex '^[a-zA-Z]{0,18}$'"
}

function main
{
    az account set --name $subscription | Out-Host
    $flags | Out-Host

    if($flags -contains "all" -or $flags -contains "group-deploy")
    {
        initializeGroup
    }

    if($flags -contains "all" -or $flags -contains "code-build")
    {
        publishSource $webJob1Name "Deploy\$webjob1Name.zip"
        publishSource $webJob2Name "Deploy\$webjob2Name.zip"
        
        # Build message sender tool
        Write-Host "Building AzureTestHarness ServiceBusSender message sender tool source" -ForegroundColor Green
        dotnet publish "..\ServiceBusSender" -p:PublishProfile=FolderProfile -c Release | out-host
    }

    $deployOut = ""

    if($flags -contains "all" -or $flags -contains "template-deploy")
    {
        $deployOut = armDeploy

        if($flags -contains "all" -or $flags -contains "code-deploy")
        {
            deploySource $deployOut.jobSite1Name $deployOut.jobSite1ResourceId $deployOut.preProductionSlotName ".\Deploy\$webjob1Name.zip"
            Write-Host "Deployment $($deployOut.jobSite1Name) complete"
            deploySource $deployOut.jobSite2Name $deployOut.jobSite2ResourceId $deployOut.preProductionSlotName ".\Deploy\$webjob2Name.zip"
            Write-Host "Deployment $($deployOut.jobSite2Name) complete"
        }
    }

    if($flags -contains "code-deploy" -and $flags -notcontains "template-deploy")
    {
        Write-Error "Unable to deploy code without deploying template"
    }

    #az webapp webjob continuous start --name $deployOut.siteName --resource-group $groupName --webjob-name $webJobName | out-host
    
    return $deployOut
}

function initializeGroup
{
    if($(az group exists --name $groupName) -eq $true){
        Write-Host "Group $groupName exists delete and recreate from template? (y/n)" -ForegroundColor Green
        az group delete --name $groupName | out-host
    }
    
    if($(az group exists --name $groupName) -eq $false){
        Write-Host "Creating Group $groupName" -ForegroundColor Green
        az group create --name $groupName --location westus | out-host
    }
}

function armDeploy
{
    #$existing = az webapp config appsettings list --name "$($appName)-web" --resource-group $groupName --subscription $subscription | convertfrom-json `
        # Translate into just name/value so that the ARM template can dedup by comparing
        #| Select-Object name,value

        
    $parameters = @{
        appName = @{
            value = $appName
        }
        #appSettings = @{
        #    value = @{
        #        settings = $existing
        #    }
        #}
    }

    $parametersJson = $parameters | ConvertTo-Json -Compress -Depth 10 | ConvertTo-Json
    write-host $parametersJson
    Write-Host "Deploying ARM template to Resource Group '$groupName'" -ForegroundColor Green
    $deployResult = az deployment group create --name testHarness --resource-group $groupName --parameters $parametersJson --template-file $tf | convertfrom-json
    if(-not $deployResult -or -not $deployResult.properties -or -not $deployResult.properties.outputs)
    {
        Write-Host $deployResult
        throw "ARM template deployment failed $?"
    }
    $deployOut = $deployResult.properties.outputs
    write-host ($deployOut | convertto-json -Depth 10)
    # Flatten deploy out object
    $deployOut | get-member -membertype properties | ForEach-Object{ $deployOut.$($_.Name) = $deployOut.$($_.Name).value }
    return $deployOut
}

function publishSource
{
    param(
        $deployProjectFolder,
        $deployLocation
    )
    # Web Job publishes into .\publish\App_Data\jobs\continuous\ServiceBusProcessor of Web Site deploy folder
    Write-Host "Building AzureTestHarness Web Job $deployProjectFolder source" -ForegroundColor Green
    dotnet publish "..\$deployProjectFolder" -p:PublishProfile=FolderProfile -c Release | out-host
    # Create zip deploy for web site and web job
    Compress-Archive "Deploy\publish\$deployProjectFolder\*" $deployLocation -Force | out-host
}

function deploySource
{
    param(
        [string] $siteName,
        [string] $siteResourceId,
        [string] $deploymentSlot,
        [string] $zipLocation
    )

    if($deploymentSlot){
        Write-Host "Deploying source to $siteName/$deploymentSlot" -ForegroundColor Green
        az webapp deployment source config-zip --ids $siteResourceId --slot $deploymentSlot --src $zipLocation | out-host
        if(-not $?)
        {
            throw "'az webapp deployment failed"
        }
        Write-Host "Swapping slot $deploymentSlot to production" -ForegroundColor Green
        az webapp deployment slot swap --ids $siteResourceId --slot $deploymentSlot | out-host

    } else {
        Write-Host "Deploying source to $siteName" -ForegroundColor Green
        az webapp deployment source config-zip --ids $siteResourceId --src $zipLocation | out-host
    }       

    if(-not $?)
    {
        throw "'az webapp deployment failed"
    }
}

main