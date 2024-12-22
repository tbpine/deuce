all:
	dotnet build .
publish:
	rm -rf publish
	dotnet publish -c release -o publish

deploy:
	rm -rf publish
	dotnet publish -c release -o publish
	rsync -avr publish/ tong@172.105.188.223:/srv/btapm/store/
