﻿//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Blade

    open System
    open System.Collections.Generic
    open System.Web
    open Castle.Blade
    open Castle.Blade.Web
    open Castle.MonoRail
    open Castle.MonoRail.Helpers
    open Castle.MonoRail.ViewEngines

    [<Interface>]
    type IViewPage = 
        abstract member Layout : string  with get,set
        abstract member ViewContext : ViewContext  with get,set
        abstract member HelperContext : HelperContext with get,set
        abstract member RawModel : obj with get,set
        abstract member Bag : IDictionary<string,obj>  with get,set
        abstract member AppRoot : string with get, set
        abstract member ContextualAppRoot : string with get, set
        // abstract member ServiceRegistry : IServiceRegistry  with get,set
        (*
            string VirtualPath { set; }
            HttpContextBase Context { set; }
            DataContainer DataContainer { get; set; }
            ViewComponentRenderer ViewComponentRenderer { get; set; }
        *)

    [<AbstractClass>]
    type ViewPage<'TModel>() = 
        inherit WebBladePage()

        let mutable _model : 'TModel = Unchecked.defaultof<_>
        let mutable _viewctx = Unchecked.defaultof<ViewContext>
        let mutable _bag = Unchecked.defaultof<IDictionary<string,obj>>
        let mutable _helperCtx = Unchecked.defaultof<HelperContext>
        let _appRoot : Ref<string> = ref null
        let _contextualAppRoot : Ref<string> = ref null

        let initHelper (h:'a) = 
            _helperCtx.ServiceRegistry.SatisfyImports(h)
            h

        let _formtag    = lazy (initHelper (FormTagHelper(_helperCtx)))
        let _form       = lazy (initHelper (FormHelper(_helperCtx)))
        let _json       = lazy (initHelper (JsonHelper(_helperCtx)))
        let _url        = lazy (initHelper (UrlHelper(_helperCtx)))
        let _js         = lazy (initHelper (JsHelper(_helperCtx)))
        let _partial    = lazy (initHelper (PartialHelper(_helperCtx, _model, _bag)))
        let _viewcomponent = lazy (initHelper (ViewComponentHelper(_helperCtx)))

        member x.ViewCtx with get() = _viewctx and set v = _viewctx <- v
        member x.HelperCtx with get() = _helperCtx and set v = _helperCtx <- v
        member x.Model   with get() = _model   and set v = _model <- v
        member x.Bag     with get() = _bag     and set v = _bag <- v

        member x.AppRoot = !_appRoot
        member x.ContextualAppRoot = !_contextualAppRoot

        // helpers
        member x.Url        = _url.Force()
        member x.FormTag    = _formtag.Force()
        member x.Form       = _form.Force()
        member x.Json       = _json.Force()
        member x.Js         = _js.Force()
        member x.Partial    = _partial.Force()
        member x.ViewComponent = _viewcomponent.Force()

        override x.PreRenderPage() = 
            x.ViewCtx.Writer <- x.Output
            base.PreRenderPage()

        override x.ConfigurePage (parent) = 
            let parent_as_vp = parent |> box :?> IViewPage
            x.ViewCtx   <- parent_as_vp.ViewContext
            x.HelperCtx <- parent_as_vp.HelperContext
            x.Model     <- parent_as_vp.RawModel |> box :?> 'TModel
            x.Bag       <- parent_as_vp.Bag
            _appRoot    := parent_as_vp.AppRoot
            _contextualAppRoot := parent_as_vp.ContextualAppRoot

        interface IViewPage with 
            member x.Layout with get() = base.Layout and set v = base.Layout <- v
            member x.ViewContext with get() = _viewctx and set v = _viewctx <- v
            member x.HelperContext with get() = _helperCtx and set v = _helperCtx <- v
            member x.RawModel with get() = _model |> box  and set v = _model <- v :?> 'TModel
            member x.Bag with get() = _bag and set v = _bag <- v
            member x.AppRoot with get() = !_appRoot and set(v) = _appRoot := v
            member x.ContextualAppRoot with get() = !_contextualAppRoot and set(v) = _contextualAppRoot := v

    
    [<AbstractClass>]
    type ViewPage() = 
        inherit ViewPage<System.Object>()



