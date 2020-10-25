DOTNET='/mnt/c/Program Files/dotnet/dotnet.exe'
SRC_FILES=$(shell find src -name *.cs)

.PHONY: clean
clean:
	$(DOTNET) clean -noLogo -clp:NoSummary

.PHONY: build
build: $(SRC_FILES)
	$(DOTNET) build -noLogo -clp:NoSummary

.PHONY: build-release
build-release: $(SRC_FILES)
	$(DOTNET) build -noLogo -clp:NoSummary -c Release

.PHONY: run
run: $(SRC_FILES)
	$(DOTNET) run

.PHONY: run-release
run-release: $(SRC_FILES)
	$(DOTNET) run -c Release

.PHONY: run-trace
run-trace: $(SRC_FILES)
	$(DOTNET) run -c Release < /dev/null > /dev/null &
	./scripts/trace c-sharp-ui
