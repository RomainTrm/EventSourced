module Tests 

open Domain
open Xunit
open Swensen.Unquote

let Given = id
let When (eventProducer : EventStore.EventProducer<Event>) = eventProducer
let Then expectedEvents events = test <@ expectedEvents = events @>

module SellFlavour =

    [<Fact>]
    let ``Falvour sold happy path`` () =
        Given [FlavourRestocked (Vanilla, 3)]
        |> When (Behaviour.sellFlavor Vanilla)
        |> Then [FlavourSold Vanilla]
    
    [<Fact>]
    let ``Falvour sold and went out of stock`` () =
        Given [FlavourRestocked (Vanilla, 1)]
        |> When (Behaviour.sellFlavor Vanilla)
        |> Then [FlavourSold Vanilla; FlavourWentOutOfStock Vanilla]
        
    [<Fact>]
    let ``Falvour was not in stock`` () =
        Given [FlavourRestocked (Vanilla, 4)]
        |> When (Behaviour.sellFlavor Chocolate)
        |> Then [FlavourWasNotInStock Chocolate]    