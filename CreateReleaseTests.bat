pushd "%~dp0"
rmdir "ReleaseTests" /s /q
mkdir "ReleaseTests"
copy "*.yxmd" "ReleaseTests\"
copy "*.yxmc" "ReleaseTests\"
copy "*.csv" "ReleaseTests\"
copy "*.xlsx" "ReleaseTests\"
popd
