/// <reference types="react" />
import React from "react";
export interface IProps {
    culture: string;
    onClick?: () => void;
    title?: string;
}
declare const Flag: React.SFC<IProps>;
export default Flag;
