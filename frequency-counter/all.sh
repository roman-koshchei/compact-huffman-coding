dotnet build -c Release 

EXE="./bin/Release/net8.0/FrequencyCounter.exe" 

FILES=(
  "../datasets/txt/automata-old-and-new.txt" 
  "../datasets/txt/springtime-and-other-essays.txt" 
  "../datasets/txt/stories-from-virgil.txt" 
  "../datasets/txt/tradition.txt" 
  "../datasets/txt/wit-humor-reason.txt" 
)

for file in "${FILES[@]}"; do
  "$EXE" analyze "$file" &
done

wait

echo "Analyzed all files."

"$EXE" aggregate