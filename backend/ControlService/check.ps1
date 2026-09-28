# Formats, builds and tests the solution, and prints only what matters: problems and summaries.
# Agents use it instead of the separate commands, so tool output does not flood their context.
#
#   ./check.ps1                                                   # whole solution
#   ./check.ps1 -Project tests/ControlService.Domain.Tests -Filter '*Cpf*'
#
# From Git Bash: powershell.exe -NoProfile -File backend/ControlService/check.ps1
param(
    [string]$Project,
    [string]$Filter
)

# Continue, not Stop: dotnet writes progress to stderr, which Windows PowerShell 5.1 treats as an error.
$ErrorActionPreference = 'Continue'
Set-Location $PSScriptRoot

Write-Output '== format'
# Fixes instead of verifying: files written by agents come out with LF, and .editorconfig requires CRLF.
dotnet format ControlService.slnx
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Output '== build'
dotnet build ControlService.slnx -v q -clp:Summary -nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Output '== test'
$testArguments = if ($Project) { @('--project', $Project) } else { @('--solution', 'ControlService.slnx') }
if ($Filter) { $testArguments += @('--filter-method', $Filter) }
$output = dotnet test @testArguments --no-build 2>&1
$testExitCode = $LASTEXITCODE

# A failure keeps the whole output: it is the evidence of a Red. A success needs only the summary.
if ($testExitCode -eq 0) { $output | Select-Object -Last 6 } else { $output }
exit $testExitCode
