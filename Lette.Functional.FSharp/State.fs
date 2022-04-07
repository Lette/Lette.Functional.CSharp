module State

let inc x state = (x + 1, state + "+1")
let dec x state = (x - 1, state + "-1")
let dbl x state = (x * 2, state + "*2")

let result =
    (3, "State: ")
    ||> inc
    ||> dbl
    ||> dec

let result' =
    (3, "State: ")
    ||> inc
    ||> dbl
    ||> dec

type Maybe<'a> =
    | Just of 'a
    | Nothing

module Maybe =

    let map f mt =
        match mt with
        | Nothing -> Nothing
        | Just t -> Just (f t)


    // list.Select(x => ...)
    //  ma map f
    //  ma "+" f

    let inc  x = x + 1

    let (<+>) mt f = map f mt

    let valueOrDefault d mt =
        match mt with
        | Nothing -> d
        | Just t -> t

    let mreturn x = Just x

    let apply mf mx =
        match mf with
        | Nothing -> Nothing
        | Just f ->
            match mx with
            | Nothing -> Nothing
            | Just x -> Just (f x)




type State<'State, 'T> = State of ('State -> 'T * 'State)

let fInc' x = State (fun state -> (x + 1, state + "+1"))
let fDec' x = State (fun state -> (x - 1, state + "-1"))
let fDouble' x = State (fun state -> (2 * x, state + "*2"))

let apply state m =
    let (State f) = m
    f state

// bind :: m a -> (a -> m b) -> m b
let bind m f =
    State (fun state ->
        let (State g) = m
        let (x, state') = g state
        let (State h) = f x
        h state'
    )

let unit a = State (fun state -> (a, state))

let (>>=) = bind

let result'' =
    unit 42
    >>= fDouble'
    >>= fDouble'
    >>= fDec'
    >>= fDouble'
    >>= fDouble'
    >>= fDouble'
    >>= fInc'

let result''' = result'' |> apply "waat! "


//let F () = ()
