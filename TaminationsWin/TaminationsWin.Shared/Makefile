
#  This file generates the xml files in the raw directory from
#  the corresponding files from the Taminations project.
#  Requires $(TAMINATIONS) to be set to the location of that project
#  On Windows, use Cygwin

#  These are the files to copy from Taminations to the app
TAMDIRS = b1 b2 ms plus a1 a2 c1 c2 c3a c3b
TAMTYPES = xml html png css dtd 
SRCTAM = $(foreach dir,$(TAMDIRS),\
         $(foreach type,$(TAMTYPES),\
         $(wildcard $(TAMINATIONS)/$(dir)/*.$(type))))
SRCDIRFILES = callindex.xml calls.xml formations.xml moves.xml tamination.dtd tutorial.xml
SRC = $(SRCTAM) $(addprefix src/,$(SRCDIRFILES))

#  Generate destinations filename from source filenames
SRCNAMES = $(subst $(TAMINATIONS)/,,$(SRC))

OBJDIR = assets
OBJ = $(subst /ms/,/ms0/,$(addprefix $(OBJDIR)/,$(SRCNAMES)))
OBJHTML = $(filter-out assets/ms0/%,$(filter %.html,$(OBJ)))
OBJCODE = $(filter-out assets/ms0/%,$(filter-out %.html,$(OBJ)))
OBJMSHTML = $(filter assets/ms0/%,$(filter %.html,$(OBJ)))
OBJMSCODE = $(filter assets/ms0/%,$(filter-out %.html,$(OBJ)))

PREVOBJ = $(filter-out assets/info/about.html assets/info/sequencer.html,$(wildcard $(OBJDIR)/*/*))

#  Dependencies
.PHONY: all clean
all : $(OBJ) $(OBJDIR)/src/tamination.css

COPY = cp $< $@
#  Remove overflow:hidden from css so WebView will scroll
$(OBJDIR)/src/tamination.css : $(TAMINATIONS)/src/tamination.css
	perl -p -e "s/(overflow: hidden;)//" $< >$@
#  WebView needs the viewport meta tag for proper scaling, so keep it
#  But remove scripts
COPYHTML = perl -p -e "s/^<.*(script|favicon).*//" $< >$@
$(OBJHTML) : assets/%.html : $(TAMINATIONS)/%.html
	$(COPYHTML)
$(OBJMSHTML) : assets/ms0/%.html : $(TAMINATIONS)/ms/%.html
	$(COPYHTML)
$(OBJCODE) : assets/%: $(TAMINATIONS)/%
	$(COPY)
$(OBJMSCODE) : assets/ms0/%: $(TAMINATIONS)/ms/%
	$(COPY)

clean :
	-rm $(PREVOBJ)
