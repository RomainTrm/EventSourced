// Learn more about F# at http://fsharp.org

module PrintHelper = 
    open Domain

    let printHistory events =
        printfn "History"
        
        events
        |> List.iteri (fun i event -> printfn " %i: %A" (i + 1) event)
        
        printfn ""

    let printStock (stocks:Map<Flavour, int>) =
        printfn "Stocks"

        stocks
        |> Map.iter (printfn " Flavour: %A, In stock: %i") 


open EventStore
open Domain

[<EntryPoint>]
let main argv =
    let eventStore : EventStore<Event> = createInstance()

    eventStore.Append [FlavourRestocked (Vanilla, 5); FlavourRestocked (Chocolate, 5); FlavourRestocked (Strawberry, 1)]
    eventStore.Append [FlavourSold Chocolate; FlavourSold Vanilla]
    eventStore.Append [FlavourSold Chocolate]
    eventStore.Append [FlavourSold Strawberry; FlavourWentOutOfStock Strawberry]
    eventStore.Append [FlavourSold Chocolate; FlavourWasNotInStock Strawberry]

    let history = eventStore.GetHistory ()
    PrintHelper.printHistory history

    let stocks = history |> project flavorStocks
    PrintHelper.printStock stocks

    0 // return an integer exit code
