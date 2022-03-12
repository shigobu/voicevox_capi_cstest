# voicevox_capi_cstest
voicevoxのC言語apiを使ってみるテスト

## ビルド方法
Visual Studio のビルドを使用  
.NET 6を使用しているので、dotnetコマンドが使えるはずです。  
```
dotnet build --framework net6.0-windows
```

## 実行方法
~~出力ディレクトリに、各種onnxファイル・core.dll・onnxruntime.dllを配置してください。~~  
出力ディレクトリにcore.zipの中身とonnxruntime.dllを配置してください。  

OpenJTalkの辞書は「出力ディレクトリ\open_jtalk_dic_utf_8-1.11」に配置してください。

出力ディレクトリに有るvoicevox_capi_cstest.exeをダブルクリックで起動できます。  
dotnetコマンドも使えるはずです。  
```
dotnet run --framework net6.0-windows
```

## 開発環境
IDE : Visual Studio Community 2022  
フレームワーク : .NET 6  
