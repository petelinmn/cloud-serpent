--drop table aggregationrules
create sequence aggregationrules_id_seq as integer;
create table aggregationrules
(
    id            integer       default nextval('aggregationrules_id_seq'::regclass)    not null primary key,
    name          text unique                                                           not null,
    atomics       json,
    contextfields text[],
    condition     text,
    timespan      real
);