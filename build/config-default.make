# -*- makefile -*-
#
# This makefile fragment has (default) configuration
# settings for building MCS.

# DO NOT EDIT THIS FILE! Create config.make and override settings
# there.

RUNTIME_FLAGS = 
TEST_HARNESS = $(topdir)/class/lib/$(PROFILE)/nunit-console.exe
MCS_FLAGS = $(PLATFORM_DEBUG_FLAGS)
MBAS_FLAGS = $(PLATFORM_DEBUG_FLAGS)
LIBRARY_FLAGS = /noconfig
CFLAGS = -g -O2
INSTALL = /usr/bin/install
prefix = /usr/local
RUNTIME = mono $(RUNTIME_FLAGS)
TEST_RUNTIME = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) --debug

# In case you want to add MCS_FLAGS, this lets you not have to
# keep track of the default value

DEFAULT_MCS_FLAGS := $(MCS_FLAGS)
DEFAULT_MBAS_FLAGS := $(MBAS_FLAGS)

# You shouldn't need to set these but might on a 
# weird platform.

# CC = cc
# SHELL = /bin/sh
# MAKE = gmake 
