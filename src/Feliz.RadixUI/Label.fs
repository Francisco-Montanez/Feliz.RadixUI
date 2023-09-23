namespace Feliz.RadixUI.Label

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.RadixUI
open Browser.Types


/// Renders an accessible label associated with controls.
type label =
    /// Contains the content for the label.
    static member inline root (props: IReactProperty seq) = createElement (import "Root" "@radix-ui/react-label") props


/// Contains the content for the label.
type root =
    /// Change the default rendered element for the one passed as a child, merging their props and behavior.  Read our Composition guide for more details.
    static member inline asChild (value: bool) = Feliz.Interop.mkAttr "asChild" value
    /// The id of the element the label is associated with.
    static member inline htmlFor (value: string) = Feliz.Interop.mkAttr "htmlFor" value



