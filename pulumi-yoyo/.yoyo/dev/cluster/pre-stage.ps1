Write-Output "Running pre-stage.ps1 - will check if the cluster is running and start/stop is as required"
Write-Output "Full stack name: $(${env:YOYO_STACK_FULL_STACK_NAME})"

$ErrorActionPreference = "Stop"

$envStackName = & pulumi stack -s $env:YOYO_STACK_FULL_STACK_NAME output EnvironmentStackName 
if($LASTEXITCODE -ne 0)
{
    Write-Output "Error getting EnvironmentStackName - exiting with 'continue'"
    exit 100
}

$clusterName = & pulumi stack -s $env:YOYO_STACK_FULL_STACK_NAME output ClusterName
if($LASTEXITCODE -ne 0)
{
    Write-Output "Error getting ClusterName - exiting with 'continue'"
    exit 100
}

$clusterRg = & pulumi stack -s $envStackName output ClusterResourceGroup
if($LASTEXITCODE -ne 0)
{
    Write-Output "Error getting ClusterResourceGroup - exiting with 'continue'"
    exit 100
}

$cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
if ($null -eq $cluster)
{
    Write-Output "Cluster not found - exiting with 'continue'"
    exit 100
}

if ($env:YOYO_STAGE -eq "destroy")
{
    $desiredState = "Stopped"
    $action = { Stop-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg }
}
else
{
    $desiredState = "Running"
    $action = { Start-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg }
}

while($cluster.PowerState.Code -ne $desiredState)
{
    Invoke-Command -ScriptBlock $action -ErrorAction Stop
    Write-Output "Waiting for cluster ${clusterName} to be in state: $desiredState"
    Start-Sleep -Seconds 10
    $cluster = Get-AzAksCluster -Name $clusterName -ResourceGroupName $clusterRg
}

Write-Output "Cluster is now in state: $($cluster.PowerState.Code)"

# if we are doing a "destroy" - and we just stopped the cluster - exit with a 0
# this will stop the yoyo from processing the job chain, which makes sense because
# there is no sense trying to pulumi destroy a cluster that is no longer running.
if ($env:YOYO_STAGE -eq "destroy")
{
    exit 0
}

# if we get here, the cluster is stopped - so exit out with a 100 - this exit
# code forces the yoyo to continue processing the job chain.
exit 100
 
