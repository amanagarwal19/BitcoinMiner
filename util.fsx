
open System.Net
open System.Text
open System
open System.Security.Cryptography
open System.Threading
open System.IO



// -------------------  UTILITIES -------------------------------

let encrypt (input:string) =
    let author = input
    let inputBytes = Encoding.ASCII.GetBytes(author)
    let mySha = SHA256.Create()

    let output = mySha.ComputeHash(inputBytes)
    let stringVersion = BitConverter.ToString(output)

    let mutable cleanString = ""
    for i in stringVersion do
        if i <> '-' then
            cleanString <- cleanString + (string i)
    // Console.WriteLine(stringVersion)
    cleanString.ToLower()

//We assign tasks on basis of alphabet range. 
// Each worker generates random strings in the format of:
// <UFID> <Random characters in the given range of (length 5)> <random special characters and numbers (length 3)>
// So we split the alphabet range to the different workers
            
let stringGenerator (starting:int):string=
    let characterRange = "ABCDEFGHIJKLMNOPQRSTUVWUXYZ"
    let numberRange = "0123456789"
    let specialChars = ":?><';/*-+=!@#$^&*()"
    let mutable startingIndex = starting
    let mutable endingIndex = starting+4 //take 5 chars at a time
    // printfn "\n\n%d %d\n\n" startingIndex endingIndex
    // if startingIndex>endingIndex then
    //     let temp = startingIndex
    //     startingIndex <- endingIndex
    //     endingIndex <- temp
   
    let randomizer =  System.Random()
    let mutable str="83494778;"
    
    //Generate string of random characters
    for i=0 to 5 do
        let randomIndexChar = randomizer.Next(startingIndex,endingIndex+1)
        // printfn "\n\ns: %d\te:%d\n\n" startingIndex (endingIndex+1)
        let randomChar = characterRange.Chars(randomIndexChar)
        // printfn "\n\n%d %c\n\n" randomIndexChar randomChar
        str <-   str + System.Char.ToString(randomChar)
    
    // Generate string of random numbers and special characters   
    for i=0 to 3 do
        let randomNumber = numberRange.Chars(randomizer.Next(10))
        let randomSpecial = specialChars.Chars(randomizer.Next(specialChars.Length-1))
        str<-   str+
                System.Char.ToString(randomNumber)+
                System.Char.ToString(randomSpecial)
    let stringToEncrypt = str.ToLower()
    //Console.WriteLine(stringToEncrypt)
    stringToEncrypt

let validCoin (input:string,n:int):bool = 
    let validationString = input.[0..(n-1)]
    // printfn "%s" validationString
    let mutable isValid = true
    for item in validationString do
        if item <> '0' then 
            isValid <- false

    isValid

//Test validation
// printfn "%s " (if validCoin("amansnf",2) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("0amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("00amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("000amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("0000amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("00000amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("000000amansnf",4) then "Valid" else "Invalid")
// printfn "%s " (if validCoin("0042400amansnf",4) then "Valid" else "Invalid")