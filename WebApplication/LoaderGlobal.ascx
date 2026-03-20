<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoaderGlobal.ascx.cs" Inherits="WebApplication.LoaderGlobal" %>

<style>
    .global-loader {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.45);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999999;
    }

    .global-loader--hidden {
        display: none;
    }

    .global-loader__content {
        background: #fff;
        padding: 25px 30px;
        border-radius: 12px;
        text-align: center;
    }

    .global-loader__spinner {
        width: 50px;
        height: 50px;
        border: 5px solid #ddd;
        border-top: 5px solid #007bff;
        border-radius: 50%;
        animation: spinLoader 0.8s linear infinite;
        margin: 0 auto 10px;
    }

    .global-loader__text {
        font-size: 14px;
        font-weight: 600;
    }

    @keyframes spinLoader {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
</style>

<div id="globalLoader" class="global-loader global-loader--hidden">
    <div class="global-loader__content">
        <div class="global-loader__spinner"></div>
        <div id="globalLoaderText" class="global-loader__text">Procesando...</div>
    </div>
</div>