module EventStore

type Aggregate = System.Guid

type EventProducer<'Event> = 'Event list -> 'Event list

type EventStore<'Event> = 
    {
        Get : unit -> Map<Aggregate, 'Event list>
        GetStream : Aggregate -> 'Event list
        Evolve : Aggregate -> EventProducer<'Event> -> unit
    }

type private Msg<'Event> = 
| Get of AsyncReplyChannel<Map<Aggregate, 'Event list>>
| GetStream of Aggregate * AsyncReplyChannel<'Event list>
| Evolve of Aggregate * EventProducer<'Event>

let private eventsOfAggregate aggregate history =
    history 
    |> Map.tryFind aggregate
    |> Option.defaultValue []

let private mailbox = 
    MailboxProcessor.Start(fun inbox -> 
        let rec loop history = async {
            let! msg = inbox.Receive()

            match msg with 
            | Get channel -> 
                channel.Reply history
                return! loop history

            | GetStream (aggregate, channel) ->   
                channel.Reply (history |> eventsOfAggregate aggregate)
                return! loop history           

            | Evolve (aggregate, producer) -> 
                let aggregateHistory = history |> eventsOfAggregate aggregate
                let newEvents = producer aggregateHistory
                let newHistory = history |> Map.add aggregate (aggregateHistory@newEvents)
                return! loop newHistory                       
        }

        loop Map.empty)

let createInstance () : EventStore<Domain.Event> = 
    {
        Get = fun () -> mailbox.PostAndReply Get
        GetStream = fun aggregate -> mailbox.PostAndReply (fun reply -> (GetStream (aggregate, reply)))
        Evolve = fun aggregate producer -> mailbox.Post (Evolve (aggregate, producer))
    }