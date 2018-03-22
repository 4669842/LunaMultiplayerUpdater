FROM mono
EXPOSE 8800/udp
EXPOSE 8801/udp
RUN sudo apt-get update && sudo apt-get install wget
RUN wget https://github.com/4669842/LunaMultiplayerUpdater/releases/download/1.0.51/LunaManager.exe
CMD [ "mono",  "./LunaManager.exe" ]
