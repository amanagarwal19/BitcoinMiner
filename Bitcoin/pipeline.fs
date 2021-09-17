open System


let cards = [10;15;12;13;9;23]

let cardFace card = 
    let no = card%13
    if no = 1 then "Ace"
    elif no = 0  then "King"
    elif no = 11 then "Jack"
    elif no = 12 then "Queen"
    else string no

let suit card = 
    let no = card / 13
    if no = 0 then "Hearts"
    elif no = 1 then "Spades"
    elif no = 2 then "Diamonds"
    else "Clubs"

let shuffleCards list = 
    let random = System.Random()
    list |>List.sortBy (fun x->random.Next())

let printCard card = 
    printfn "%s of %s" (cardFace card) (suit card)

let printAll list = 
    List.iter(fun x->printCard(x)) list //the input parameter for the iteration



[<EntryPoint>]

let main argv = 
    
    cards |> printAll // Take all cards and print them

    cards |> shuffleCards|> printAll //Shuffle cards and print
    0