module EventStore

type EventProducer<'Event> = 'Event list -> 'Event list

type EventStore<'Event> = 
    {
        GetHistory : unit -> 'Event list
        Evolve : EventProducer<'Event> -> unit
    }

type private Msg<'Event> = 
| Get of AsyncReplyChannel<'Event list>
| Evolve of EventProducer<'Event>

let private mailbox = 
    MailboxProcessor.Start(fun inbox -> 
        let rec loop history = async {
            let! msg = inbox.Receive()

            match msg with 
            | Get channel -> 
                channel.Reply history
                return! loop history
            | Evolve producer -> 
                let newEvents = producer history
                return! loop (history@newEvents)            
        }

        loop [])

let createInstance () : EventStore<Domain.Event> = 
    {
        GetHistory = fun () -> mailbox.PostAndReply Get
        Evolve = fun producer -> mailbox.Post (Evolve producer)
    }