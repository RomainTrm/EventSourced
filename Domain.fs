module Domain

type Flavour = 
| Strawberry
| Vanilla
| Chocolate

type Event = 
| FlavourSold of Flavour
| FlavourRestocked of Flavour * int
| FlavourWentOutOfStock of Flavour
| FlavourWasNotInStock of Flavour

type Projection<'State, 'Event> = 
    {
        Init : 'State
        Update : 'State -> 'Event -> 'State
    }

let private flavourStock flavour state = 
    state
    |> Map.tryFind flavour
    |> Option.defaultValue 0

let private updateFlavourStock state event =
    match event with 
    | FlavourSold flavour -> 
        state
        |> flavourStock flavour
        |> fun stock -> Map.add flavour (stock - 1) state
    | FlavourRestocked (flavour, quantity) ->
        state
        |> flavourStock flavour
        |> fun stock -> Map.add flavour (stock + quantity) state
    | _ -> state

let flavorStocks : Projection<Map<Flavour, int>, Event> = 
    {
        Init = Map.empty
        Update = updateFlavourStock
    }

let project projection events =
    events
    |> List.fold projection.Update projection.Init