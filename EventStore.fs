module EventStore

type EventStore<'Event> = 
    {
        GetHistory : unit -> 'Event list
        Append : 'Event list -> unit
    }

type private Msg<'Event> = 
| Get of AsyncReplyChannel<'Event list>
| Save of 'Event list

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
        }

        loop [])


let createInstance () : EventStore<Domain.Event> = 
    {
        GetHistory = fun () -> mailbox.PostAndReply Get
        Append = fun events -> mailbox.Post (Save events)
    }