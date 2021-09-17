open System
open System.Security.Cryptography
open System.IO
open System.Text


let mySha = SHA256.Create()

let lines = IO.File.ReadLines "abc.txt"
let mutable valid = true

if(isNull lines) then valid<-false
else printfn "%A" lines


let bytes = IO.File.ReadAllBytes "abc.txt"


let out = mySha.ComputeHash(bytes)
let out2 = SHA256.HashData(bytes)
IO.File.WriteAllBytes("out.txt",out)
// IO.File.WriteAllLines("linesout.txt",lines)
let stringOut = BitConverter.ToString(out)
let mutable cleanString = ""
let givenHash = "0xe9a425077e7b492076b5f32f58d5eb6824b1875621e6237f1a2430c6b77e467c"
let mutable k=0
let mutable shaMatch = true
for i = 0 to stringOut.Length-1 do
    if stringOut.Chars(i)<> '-' then 
        let lower = System.Char.ToLower(stringOut.Chars(i))
        if givenHash.Chars(k) <> lower then 
            shaMatch<-false
        k<-k+1
printfn "%s" stringOut

printfn "%b" (shaMatch)