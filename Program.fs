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

    let truck1 = System.Guid.NewGuid()
    let truck2 = System.Guid.NewGuid()

    eventStore.Evolve truck1 (Behaviour.sellFlavor Vanilla)
    eventStore.Evolve truck1 (Behaviour.restockFlavor Vanilla 5)

    eventStore.Evolve truck1 (Behaviour.sellFlavor Vanilla)
    eventStore.Evolve truck1 (Behaviour.sellFlavor Strawberry)
    eventStore.Evolve truck1 (Behaviour.sellFlavor Chocolate)

    eventStore.Evolve truck2 (Behaviour.restockFlavor Chocolate 5)
    eventStore.Evolve truck2 (Behaviour.sellFlavor Strawberry)
    eventStore.Evolve truck2 (Behaviour.sellFlavor Chocolate)

    printfn "\r\nTruck 1:"
    let historyTruck1 = eventStore.GetStream truck1
    PrintHelper.printHistory historyTruck1

    let stocks = historyTruck1 |> project flavorsStocks
    PrintHelper.printStock stocks

    printfn "\r\nTruck 2:"
    let historyTruck2 = eventStore.GetStream truck2
    PrintHelper.printHistory historyTruck2

    let stocks = historyTruck2 |> project flavorsStocks
    PrintHelper.printStock stocks

    0 // return an integer exit code
