@echo off
echo Close the solutions before execute!
echo Deleting BIN,OBJ and x64 folders...
powershell.exe -command "& '.\DeleteBinAndObjFolders.ps1'"
pause