module EventStore

type EventProducer<'Event> = 'Event list -> 'Event list

type EventStore<'Event> = 
    {
        GetHistory : unit -> 'Event list
        Append : 'Event list -> unit
        Evolve : EventProducer<'Event> -> unit
    }

type private Msg<'Event> = 
| Get of AsyncReplyChannel<'Event list>
| Save of 'Event list
| Evolve of EventProducer<'Event>

let private mailbox = 
    MailboxProcessor.Start(fun inbox -> 
        let rec loop history = async {
            let! msg = inbox.Receive()

            match msg with 
            | Get channel -> 
                channel.Reply history
                return! loop history
            | Save events ->
                return! loop (history@events)
            | Evolve producer -> 
                let newEvents = producer history
                return! loop (history@newEvents)            
        }

        loop [])

let createInstance () : EventStore<Domain.Event> = 
    {
        GetHistory = fun () -> mailbox.PostAndReply Get
        Append = fun events -> mailbox.Post (Save events)
        Evolve = fun producer -> mailbox.Post (Evolve producer)
    }