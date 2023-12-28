Param($UnityPid, $UnityPath, $ProjectDir)

#$ErrorActionPreference = "Stop"

$hubPath = (Get-ItemPropertyValue -Path "HKLM:\SOFTWARE\Unity Technologies\Hub" -Name "InstallLocation")

Write-Host "UnityPid=$UnityPid, UnityPath='$UnityPath' ProjectDir='$ProjectDir', hubPath='$hubPath'"

Write-Host "Waiting for Unity process $UnityPid to exit..."
while ((Get-Process -Id $UnityPid -ErrorAction SilentlyContinue) -ne $null) {
    Write-Host -NoNewline "."
    Start-Sleep -Seconds 1
}

Write-Host ""

Write-Host "Removing script related files from the Unity library"

cd $ProjectDir

function RemoveFolder {
    param($Subfolder)

    if (Test-Path $Subfolder) {
        Write-Host "Removing folder '$Subfolder' and contents"
        Remove-Item -Path $Subfolder -Force -Recurse
    }
}

RemoveFolder "obj"

RemoveFolder "Temp"

RemoveFolder "Library\ScriptAssemblies"

RemoveFolder "Library\Bee"

RemoveFolder "Library\BurstCache"



Write-Host "Removing all project and solution files"
Remove-Item "*.csproj"
Remove-Item "*.sln"

Write-Host "Complete."
Write-Host ""
Write-Host "Starting Unity"

#Start-Process "$hubPath\Unity Hub.exe" -ArgumentList "-- --headless help" -Wait

Start-Process $UnityPath -ArgumentList "-projectpath `"$ProjectDir`""

Start-Sleep -Seconds 10
