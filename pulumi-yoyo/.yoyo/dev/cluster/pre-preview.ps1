Write-Host "Running pre-preview.ps1 - will check if the cluster is running and start it if not"
Write-Host "Full stack name:", $env:YOYO_FULL_STACK_NAME

$envStackName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output EnvironmentStackName
$clusterName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output ClusterName 
$clusterRg = & pulumi stack -s $envStackName output ClusterResourceGroup

$cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
if ($null -ne $cluster)
{
    if($cluster.PowerState.Code -eq "Running")
    {
        Write-Host "Cluster found and running - can skip pulumi up"
        return 100
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

return 0 
