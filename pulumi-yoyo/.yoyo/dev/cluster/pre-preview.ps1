Write-Host "Running pre-preview.ps1 - will check if the cluster is running and start it if not"
Write-Host "Full stack name:", $env:YOYO_FULL_STACK_NAME

$ErrorActionPreference = "Stop"

$envStackName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output EnvironmentStackName 
if($LASTEXITCODE -ne 0)
{
    Write-Host "Error getting EnvironmentStackName - cannot continue"
    exit 1
}

$clusterName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output ClusterName
if($LASTEXITCODE -ne 0)
{
    Write-Host "Error getting ClusterName - cannot continue"
    exit 1
}

$clusterRg = & pulumi stack -s $envStackName output ClusterResourceGroup
if($LASTEXITCODE -ne 0)
{
    Write-Host "Error getting ClusterResourceGroup - cannot continue"
    exit 1
}

$cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
if ($null -ne $cluster)
{
    if($cluster.PowerState.Code -eq "Running")
    {
        Write-Host "Cluster found and running - continue with the preview"
    }
    else
    {
        Write-Host "Cluster found but not running - first start it up"
        Start-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
        while($cluster.PowerState.Code -ne "Running")
        {
            Start-Sleep -Seconds 10
            $cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
        }
    }
}

exit 100 
