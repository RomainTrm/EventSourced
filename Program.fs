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

    eventStore.Evolve (Behaviour.sellFlavor Vanilla)
    eventStore.Evolve (Behaviour.restockFlavor Vanilla 5)

    eventStore.Evolve (Behaviour.sellFlavor Vanilla)
    eventStore.Evolve (Behaviour.sellFlavor Strawberry)
    eventStore.Evolve (Behaviour.sellFlavor Chocolate)

    let history = eventStore.GetHistory ()
    PrintHelper.printHistory history

    let stocks = history |> project flavorsStocks
    PrintHelper.printStock stocks

    0 // return an integer exit code
