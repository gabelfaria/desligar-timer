# Desligar Timer — edição Metro

Aplicativo Windows Forms redesenhado com uma interface inspirada no Metro Design: tipografia Segoe UI, hierarquia visual simples, cartões retangulares, paleta clara e azul de destaque, controles planos e relógio em evidência.

## Alterações

- O modo antes chamado “Cronômetro” agora aparece como **Duração**, e a configuração principal é chamada **temporizador**.
- A duração usa um picker visual com números grandes, cartões escuros e separadores de tempo, sem bordas pretas. Clique em qualquer número para editá-lo diretamente; a roda do mouse é o modo padrão para aumentar ou diminuir o valor.
- A janela foi compactada para ocupar uma pequena área da tela.
- Ao minimizar, o programa se esconde na área de notificação e exibe um alerta informando a ação.
- Ao fechar a janela pelo botão X, o programa também permanece na área de notificação e exibe um alerta.
- A janela abre inicialmente no canto inferior direito da tela.
- Durante a contagem, o tempo restante aparece nos próprios três quadrados, substituindo os valores de configuração.
- A aba **Hora exata** usa os mesmos três quadrados para hora, minuto e segundo.
- Os valores de Duração e Hora exata são preservados separadamente ao alternar entre as abas.
- O pacote inclui `Desinstalar.bat`, que remove o programa instalado e a inicialização automática do Windows.
- Na primeira execução, o aplicativo registra a si mesmo em `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, sem exigir privilégios de administrador.
- Quando iniciado pelo Windows, ele abre minimizado na área de notificação. Clique duas vezes no ícone para abrir a janela; o menu do ícone também permite abrir, cancelar o temporizador ou sair.
- Mantém os modos de duração e hora exata e as ações de desligar, suspender e hibernar.

O Windows só pode começar a iniciar o programa automaticamente depois que ele for executado uma primeira vez (ou instalado pelo arquivo `Instalar.bat`). O caminho registrado é o local do executável; mantenha-o nesse local. Temporizadores em andamento não são salvos nem restaurados após reiniciar ou sair do Windows.

## Instalação no Windows

1. Extraia os arquivos do pacote para uma pasta local.
2. Execute `Instalar.bat` uma vez. O script copia o programa para `%LOCALAPPDATA%\Programs\DesligarTimer` e o inicia; na primeira execução ele configura o início automático para os próximos logins.
3. Para remover a inicialização automática, exclua o valor `DesligarTimer` em `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.

Também é possível abrir `DesligarTimer.exe` diretamente. Nesse caso, a inicialização automática aponta para o local atual do arquivo.

## Compilação

O projeto tem como alvo .NET Framework 4.0 e pode ser aberto no Visual Studio com suporte ao .NET Framework. Em uma instalação do Mono com as referências do Windows Forms, a compilação equivalente é:

```bash
mcs -target:winexe -sdk:4 -langversion:7.2 \
  -r:System.Windows.Forms.dll -r:System.Drawing.dll \
  -win32icon:app.ico -out:DesligarTimer.exe \
  Program.cs MainForm.cs Enums.cs Properties/AssemblyInfo.cs
```
