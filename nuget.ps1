del TsSoft.Installer.*.nupkg
del *.nuspec
del .\TsSoft.Installer\bin\Release\*.nuspec

function GetNodeValue([xml]$xml, [string]$xpath)
{
	return $xml.SelectSingleNode($xpath).'#text'
}

function SetNodeValue([xml]$xml, [string]$xpath, [string]$value)
{
	$node = $xml.SelectSingleNode($xpath)
	if ($node) {
		$node.'#text' = $value
	}
}

Remove-Item .\TsSoft.Installer\bin -Recurse 
Remove-Item .\TsSoft.Installer\obj -Recurse

$build = "c:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe ""TsSoft.Installer\TsSoft.Installer.csproj"" /p:Configuration=Release" 
Invoke-Expression $build

$Artifact = (resolve-path ".\TsSoft.Installer\bin\Release\TsSoft.Installer.dll").path

nuget spec -F -A $Artifact

Copy-Item .\TsSoft.Installer.nuspec.xml .\TsSoft.Installer\bin\Release\TsSoft.Installer.nuspec

$GeneratedSpecification = (resolve-path ".\TsSoft.Installer.nuspec").path
$TargetSpecification = (resolve-path ".\TsSoft.Installer\bin\Release\TsSoft.Installer.nuspec").path

[xml]$srcxml = Get-Content $GeneratedSpecification
[xml]$destxml = Get-Content $TargetSpecification
$value = GetNodeValue $srcxml "//version"
SetNodeValue $destxml "//version" $value;
$value = GetNodeValue $srcxml "//description"
SetNodeValue $destxml "//description" $value;
$value = GetNodeValue $srcxml "//copyright"
SetNodeValue $destxml "//copyright" $value;
$destxml.Save($TargetSpecification)

nuget pack $TargetSpecification

del *.nuspec
del .\TsSoft.Installer\bin\Release\*.nuspec

exit
