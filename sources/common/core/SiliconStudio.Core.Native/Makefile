
SRC_DIR=.
OBJ_DIR=obj
SRCS_NAMES=lz4_32.cpp lz4_64.cpp lz4hc_32.cpp lz4hc_64.cpp matrix.cpp audioCallbacks.iOS.cpp
OBJS_NAMES=$(SRCS_NAMES:.cpp=.o)
SRCS=$(patsubst %,$(SRC_DIR)/%,$(SRCS_NAMES))
OBJS=$(patsubst %,$(OBJ_DIR)/%,$(OBJS_NAMES))
MAIN=$(OBJ_DIR)/$(MAIN_NAME)
CFLAGS+= -I lz4 

%.a : $(OBJS)
	$(AR) rcs $(MAIN) $(OBJS)
	ranlib $(MAIN)

%.so: $(OBJS)
	$(CC) $(LDFLAGS) $(OBJS) -o $(MAIN)

$(OBJS): $(OBJ_DIR)/%.o: $(SRC_DIR)/%.cpp
	mkdir -p $(OBJ_DIR)
	$(CC) $(CFLAGS) $< -o $@

clean:
	rm -rf $(OBJ_DIR)

$(INSTALL_DIR):
	mkdir -p $(INSTALL_DIR)

install: | $(INSTALL_DIR)
	mv $(MAIN) $(INSTALL_DIR)

.PHONY: clean

debug:
	@printf "Variable state\n"
	@printf "SRC_DIR=$(SRC_DIR)\n"
	@printf "OBJ_DIR=$(OBJ_DIR)\n"
	@printf "SRCS_NAMES=$(SRCS_NAMES)\n"
	@printf "OBJS_NAMES=$(OBJS_NAMES)\n"
	@printf "SRCS=$(SRCS)\n"
	@printf "OBJS=$(OBJS)\n"
	@printf "MAIN=$(MAIN)\n"
	@printf "PWD=$(shell pwd)\n"
