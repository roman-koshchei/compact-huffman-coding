dotnet build -c Release 

EXE="./bin/Release/net8.0/FrequencyCounter.exe"

# "$EXE" cleanup "../datasets/results/index-poluted.json" "../datasets/results/index.json"

"$EXE" compare