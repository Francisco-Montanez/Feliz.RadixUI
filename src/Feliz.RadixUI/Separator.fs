namespace Feliz.RadixUI.Separator

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.RadixUI
open Browser.Types


/// Visually or semantically separates content.
type [<Erase>] separator =
    /// The separator.
    static member inline root (props: IReactProperty seq) = createElement (import "Root" "@radix-ui/react-separator") props


/// The separator.
type [<Erase>] root =
    /// Change the default rendered element for the one passed as a child, merging their props and behavior.  Read our Composition guide for more details.
    static member inline asChild (value: bool) = Feliz.Interop.mkAttr "asChild" value
    /// The orientation of the separator.
    static member inline decorative (value: bool) = Feliz.Interop.mkAttr "decorative" value


module root =

    type [<Erase>] orientation =
        static member inline horizontal = Feliz.Interop.mkAttr "orientation" "horizontal"
        static member inline vertical = Feliz.Interop.mkAttr "orientation" "vertical"


