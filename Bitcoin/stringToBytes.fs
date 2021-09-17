open System
open System.Text
open System.Security.Cryptography

let encrypt (input:string) =
    let author = input
    let inputBytes = Encoding.ASCII.GetBytes(author)
    let mySha = SHA256.Create()

    let output = mySha.ComputeHash(inputBytes)
    let stringVersion = BitConverter.ToString(output)
    // Console.WriteLine(stringVersion)
    stringVersion


let encryptRange (starting:char,ending:char)=
    let characterRange = "ABCDEFGHIJKLMNOPQRSTUVWUXYZ"
    let numberRange = "0123456789"
    let specialChars = ":?><';/*-+=!@#$%^&*()"
    let startingIndex = characterRange.IndexOf(System.Char.ToUpper(starting))
    let endingIndex = characterRange.IndexOf(System.Char.ToUpper(ending))
    let randomizer =  System.Random()
    let mutable str="834,94778;"
    
    //Generate string of random characters
    for i=0 to 5 do
        let randomIndexChar = randomizer.Next(startingIndex,endingIndex+1)
        let randomChar = characterRange.Chars(randomIndexChar)
        str <-   str + System.Char.ToString(randomChar)
    
    // Generate string of random numbers and special characters   
    for i=0 to 3 do
        let randomNumber = numberRange.Chars(randomizer.Next(10))
        let randomSpecial = specialChars.Chars(randomizer.Next(specialChars.Length-1))
        str<-   str+
                System.Char.ToString(randomNumber)+
                System.Char.ToString(randomSpecial)
    let stringToEncrypt = str.ToLower()
    Console.WriteLine(stringToEncrypt)

    let splitStrings = str.Split(',')
    printfn "%s" (splitStrings.[1])
    0


encryptRange('a','h') |>ignore

// printfn "%s" (encrypt("aman"))