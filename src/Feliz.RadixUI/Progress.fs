namespace Feliz.RadixUI.Progress

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.RadixUI
open Browser.Types


/// Displays an indicator showing the completion progress of a task, typically displayed as a progress bar.
type progress =
    /// Contains all of the progress parts.
    static member inline root (props: IReactProperty seq) = createElement (import "Root" "@radix-ui/react-progress") props
    /// Used to show the progress visually. It also makes progress accessible to assistive technologies.
    static member inline indicator (props: IReactProperty seq) = createElement (import "Indicator" "@radix-ui/react-progress") props


/// Contains all of the progress parts.
type root =
    /// Change the default rendered element for the one passed as a child, merging their props and behavior.  Read our Composition guide for more details.
    static member inline asChild (value: bool) = Feliz.Interop.mkAttr "asChild" value
    /// The progress value.
    static member inline value (value: int) = Feliz.Interop.mkAttr "value" value
    /// The maximum progress value.
    static member inline max (value: int) = Feliz.Interop.mkAttr "max" value
    /// A function to get the accessible label text representing the current value in a human-readable format. If not provided, the value label will be read as the numeric value as a percentage of the max value.
    static member inline getValueLabel (value: (int -> int -> string)) = Feliz.Interop.mkAttr "getValueLabel" value




/// Used to show the progress visually. It also makes progress accessible to assistive technologies.
type indicator =
    /// A function to get the accessible label text representing the current value in a human-readable format. If not provided, the value label will be read as the numeric value as a percentage of the max value.
    static member inline asChild (value: bool) = Feliz.Interop.mkAttr "asChild" value



