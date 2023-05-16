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

Write-Host "Removing Unity library and other generated folders and files"

cd $ProjectDir

if (Test-Path "obj") {
    #Remove-Item -Path "obj" -Force
}

if (Test-Path "Temp") {
    #Remove-Item -Path "Temp" -Force
}

if (Test-Path "Library") {
    #Remove-Item -Path "Library" -Force
}

#Remove-Item "*.csproj"
#Remove-Item "*.sln"

Read-Host

#Start-Process "$hubPath\Unity Hub.exe" -ArgumentList "-- --headless help" -Wait

Write-Host "Starting Unity"

Start-Process $UnityPath -ArgumentList "-projectpath `"$ProjectDir`""


Read-Host