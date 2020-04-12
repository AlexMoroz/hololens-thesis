#sudo apt-get install python-pip python-dev
#pip install Flask
#pip install RPi.GPIO
sudo apt-get install python-smbus
sudo date -s "$(wget -qSO- --max-redirect=0 google.com 2>&1 | grep Date: | cut -d' ' -f5-8)Z"
python app.py
