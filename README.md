# Body Tracking Unity + PythonCV

Este projeto integra rastreamento corporal em tempo real usando Python (OpenCV + cvzone) e Unity, permitindo visualização e análise de movimentos humanos para aplicações como contagem de exercícios físicos.

## Ideia

- **PythonCV**: Captura vídeo da webcam, detecta a pose corporal, calcula ângulos e envia os dados via UDP em formato JSON.
- **Unity**: Recebe os dados do Python, exibe os pontos do corpo, calcula métricas e apresenta uma interface interativa para exercícios (ex: contador de bíceps).

## Estrutura

```
PythonCV/
  main.py
Unity/
  (Assets, Scripts, Cena, etc.)
```

## Instalação

### 1. PythonCV

- Instale Python 3.8+.
- Instale as dependências:
  ```powershell
  pip install opencv-python cvzone
  ```
- Execute o script:
  ```powershell
  python PythonCV/main.py
  ```

### 2. Unity

- Abra a pasta do projeto no Unity (versão recomendada: 2021+).
- Certifique-se que o script `UDPReceive.cs` está configurado para receber na mesma porta UDP (padrão: 5052).
- Execute a cena principal.

## Como funciona

1. **PythonCV/main.py** abre a webcam, detecta a pose e envia os dados dos pontos do corpo (landmarks) e ângulos via UDP.
2. **Unity** recebe os dados, atualiza a visualização dos pontos do corpo e exibe métricas e interface de exercícios.
3. O usuário pode acompanhar os movimentos, contagem de repetições e feedback visual em tempo real.

## Requisitos

- Python 3.8+
- OpenCV (`opencv-python`)
- cvzone
- Unity 2021 ou superior

## Observações

- Certifique-se que ambos (PythonCV e Unity) estão rodando na mesma máquina ou rede local.
- A porta UDP deve ser igual nos dois lados (padrão: 5052).
- O projeto Unity depende dos dados enviados pelo PythonCV para funcionar corretamente.

## Licença

MIT

---

Dúvidas ou sugestões? Abra uma issue!