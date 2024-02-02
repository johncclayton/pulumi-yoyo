# Example pre-destroy script. 
#
# If this is a cluster, we can just stop it - and thats it - no need to delete it.
#
 
if(-not $env:YOYO_FULL_STACK_NAME)
{
    Write-Host "YOYO_FULL_STACK_NAME is not set - we cannot continue"
    return 1
}

Write-Host "Running pre-destroy.sh for ${env:YOYO_FULL_STACK_NAME} - will check if the cluster is running and just stop it rather than deleting it"

$envStackName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output EnvironmentStackName
$clusterName = & pulumi stack -s $env:YOYO_FULL_STACK_NAME output ClusterName 
$clusterRg = & pulumi stack -s $envStackName output ClusterResourceGroup

$cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
if ($null -ne $cluster)
{
    if($cluster.PowerState.Code -eq "Running")
    {
        Write-Host "Cluster found and running - we will stop it and then stop further scripts"
        Stop-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg

        while($cluster.PowerState.Code -ne "Stopped")
        {
            Start-Sleep -Seconds 10
            $cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
        }
    }

    # if we get here, the cluster is stopped - so exit out with a 100 - as we did our job...
    return 100
}

return 0 
