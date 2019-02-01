module Tests 

open Domain
open Xunit
open Swensen.Unquote

[<Fact>]
let ``SellFlavour: FalvourSold happy path`` () =
    let events = [FlavourRestocked (Vanilla, 3)]
    let newEvents = Behaviour.sellFlavor Vanilla events
    let expected = [FlavourSold Vanilla]
    test <@ expected = newEvents @>