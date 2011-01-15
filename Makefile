CSCOMPILE=mcs
CSCOMPILE_FLAGS=-debug
RUNTIME=mono
RUNTIME_FLAGS=--debug

all: Mono.CodeDom.Generic.dll

generic-codedom-generator.exe: generic-codedom-generator.cs
	$(CSCOMPILE) $(CSCOMPILE_FLAGS) generic-codedom-generator.cs

generic-codedom.generated.cs : generic-codedom-generator.exe
	$(RUNTIME) $(RUNTIME_FLAGS) ./generic-codedom-generator.exe

Mono.CodeDom.Generic.dll : generic-codedom.cs generic-codedom.generated.cs
	$(CSCOMPILE) $(CSCOMPILE_FLAGS) -debug -t:library -out:Mono.CodeDom.Generic.dll generic-codedom.cs generic-codedom.generated.cs

clean:
	rm Mono.CodeDom.Generic.dll Mono.CodeDom.Generic.dll.mdb generic-codedom.generated.cs generic-codedom-generator.exe generic-codedom-generator.exe.mdb
