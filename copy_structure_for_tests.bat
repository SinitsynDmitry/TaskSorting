@echo off
set SOURCE=E:\My\TaskSorting\RowsSorter
set DEST=E:\My\TaskSorting\RowsSorterTests

:: Ensure destination exists
if not exist "%DEST%" mkdir "%DEST%"

:: Use robocopy to copy directory structure while excluding bin and obj
robocopy "%SOURCE%" "%DEST%" /E /XD bin obj /NFL /NDL /NJH /NJS /NP /XF *.*

echo Folder structure copied from %SOURCE% to %DEST% (excluding bin and obj).
pause
