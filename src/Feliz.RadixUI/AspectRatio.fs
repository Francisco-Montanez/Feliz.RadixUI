namespace Feliz.RadixUI.AspectRatio

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.RadixUI
open Browser.Types


/// Displays content within a desired ratio.
type aspectRatio =
    /// Contains the content you want to constrain to a given ratio.
    static member inline root (props: IReactProperty seq) = createElement (import "Root" "@radix-ui/react-aspect-ratio") props


/// Contains the content you want to constrain to a given ratio.
type root =
    /// Change the default rendered element for the one passed as a child, merging their props and behavior.  Read our Composition guide for more details.
    static member inline asChild (value: bool) = Feliz.Interop.mkAttr "asChild" value
    /// The desired ratio
    static member inline ratio (value: int) = Feliz.Interop.mkAttr "ratio" value



