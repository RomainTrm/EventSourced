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

let private stockOf flavour state = 
    state
    |> Map.tryFind flavour
    |> Option.defaultValue 0

let private changeStock flavour quantity stock = 
    stock
    |> stockOf flavour
    |> fun quantityInStock -> stock |> Map.add flavour (quantityInStock + quantity)

let private updateFlavourStock stock event =
    match event with 
    | FlavourSold flavour 
        -> stock |> changeStock flavour -1
    | FlavourRestocked (flavour, quantity) 
        -> stock |> changeStock flavour quantity
    | _ -> stock

let flavorsStocks : Projection<Map<Flavour, int>, Event> = 
    {
        Init = Map.empty
        Update = updateFlavourStock
    }

let project projection =
    List.fold projection.Update projection.Init

module Behaviour =

    let sellFlavor flavour events = 

        let flavourStock = 
            events 
            |> project flavorsStocks 
            |> stockOf flavour

        match flavourStock with
        | 0 -> [FlavourWasNotInStock flavour]
        | 1 -> [FlavourSold flavour; FlavourWentOutOfStock flavour]
        | _ -> [FlavourSold flavour]

    let restockFlavor flavour quantity events =
        [FlavourRestocked (flavour, quantity)]