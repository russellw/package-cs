clang-format -i --style=file *.cs||exit /b
call clean-cs -i Program.cs||exit /b
git diff
