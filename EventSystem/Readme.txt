Manter a resolução do projeto em 640x360

Sistema para substituir OnMouseDown:
Colocar o prefab Vale EventSystem na cena e puxar a câmera para o field da classe, não precisa se preocupar com a variável Window Panel. Depois é só colocar o componente ValeTouchReceiver no objeto que precisa receber os comandos do touch, e daí é só usar os eventos no inspector.


Sistema para mudança de línguagem:

Usar a variável LanguageSettings.activeLanguage para saber qual linguagem está ativa.
