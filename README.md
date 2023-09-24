# Feliz.RadixUI
Feliz-style Fable bindings for Radix-UI.

Here is how it looks like:

```fsharp
module App

open Feliz
open Feliz.RadixUI.Accordion

let my_accordion =
    accordion.root [
        root.collapsible true
        root.defaultValue "item-1"
        root.type'.single
        prop.children [
            accordion.item [
                prop.value "item-1"
                prop.children [
                    accordion.trigger [
                        prop.text "is it accessible?"
                    ]
                    accordion.content [
                        prop.text "yes"
                    ]
                ]
            ]
            accordion.item [
                prop.value "item-2"
                prop.children [
                    accordion.trigger [
                        prop.text "is it responsive?"
                    ]
                    accordion.content [
                        prop.text "yes"
                    ]
                    ]
                ]
            ]
        ]
```