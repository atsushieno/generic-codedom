VERSION=0.1.1
DISTNAME=generic-codedom-$(VERSION)
CSCOMPILE=mcs
CSCOMPILE_FLAGS=-debug
RUNTIME=mono
RUNTIME_FLAGS=--debug

SOURCES= generic-codedom.cs generic-codedom-generator.cs

all: Mono.CodeDom.Generic.dll

generic-codedom-generator.exe: generic-codedom-generator.cs
	$(CSCOMPILE) $(CSCOMPILE_FLAGS) generic-codedom-generator.cs

generic-codedom.generated.cs : generic-codedom-generator.exe
	$(RUNTIME) $(RUNTIME_FLAGS) ./generic-codedom-generator.exe

Mono.CodeDom.Generic.dll : generic-codedom.cs generic-codedom.generated.cs
	$(CSCOMPILE) $(CSCOMPILE_FLAGS) -debug -t:library -out:Mono.CodeDom.Generic.dll generic-codedom.cs generic-codedom.generated.cs

clean:
	rm Mono.CodeDom.Generic.dll Mono.CodeDom.Generic.dll.mdb generic-codedom.generated.cs generic-codedom-generator.exe generic-codedom-generator.exe.mdb

dist:
	mkdir $(DISTNAME)
	cp $(SOURCES) $(DISTNAME)
	tar jcf $(DISTNAME).tar.bz2 $(DISTNAME)

cleanup-dist:
	rm -rf $(DISTNAME).tar.bz2 $(DISTNAME)


dogfood: generic-codedom-generator-dogfood.cs
	mcs -debug -r:Mono.CodeDom.Generic.dll generic-codedom-generator-dogfood.cs
	mono --debug generic-codedom-generator-dogfood.exe

clean-dogfood:
	rm generic-codedom-generator-dogfood.exe generic-codedom-generator-dogfood.exe.mdb generic-codedom.generated.dogfood.cs
