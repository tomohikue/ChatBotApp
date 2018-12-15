# ChatBotApp
Bot Framework SDKで作成されたQnA Maker用のコードサンプル

このコードは、QnA Maker連携のBot Serviceを標準設定で作ったテンプレートに対して、以下の機能を追加したものです。

- 1問多答 (Scoreが閾値以上のものを候補する。設定した返答の数を返す。)
- チャットボットの会話フローを作る。（元のサンプルは質問文を入れると回答が返ってくるだけで、そっけなさすぎるので少し会話を作りました。
- Application Insightに書き込まれる会話ログが少なすぎるので、追加しました。

## 環境情報
- C#
- Visual Studio 2017
- .Net Framework (.Net Coreではない）

## 使い方

①Visual Studioでソリューションファイル「ChatBotApp.sln」を開く

②Web.configを修正する

~~~xml
  <appSettings>
    <!--Bot Serviceから取得する情報-->
    <add key="MicrosoftAppId" value="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx" />
    <add key="MicrosoftAppPassword" value="pass" />
    <!--QnA Makerから取得する情報-->
    <add key="QnAAuthKey" value="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx" />
    <add key="QnAKnowledgebaseId" value="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx" />
    <add key="QnAEndpointHostName" value="https://xxxxxxxx.azurewebsites.net/qnamaker" />
    <!--返却する回答数-->
    <add key="QnAGetCnt" value="3" />
    <!--返却する回答のScoreの閾値-->
    <add key="QnAConfidenceThreshold" value="50" />
  </appSettings>
~~~

③ApplicationInsights.configを修正する
~~~xml
  <!--QnA Makerで使っているApplication Insightのキー-->
  <InstrumentationKey>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx</InstrumentationKey>
~~~

④ビルドして実行

⑤Bot Framework エミュレータでテスト

